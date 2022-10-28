using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using AspNetCore.Totp;
using MFA_POC.Model;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text.Json;
using System.IO;
using MimeKit.Utils;
using CoreHtmlToImage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


namespace MFA_POC.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MFAController : ControllerBase
    {
        private readonly UsersDbContext _context;
        private readonly IServiceScopeFactory _services;
        private readonly IConfiguration Configuration;
        public MFAController(UsersDbContext context, IServiceScopeFactory services, IConfiguration configuration) {
            this.Configuration = configuration;
            this.generator = new TotpGenerator();
            this.validator = new TotpValidator(this.generator);
            _context = context;
            _services = services;
        }
        private TotpGenerator generator { get; set; }
        private TotpValidator validator { get; set; }

                  

        [HttpPost] //{userId:[userId], issuer: [project_name]}
        public ActionResult GetURL([FromBody] Payload payload)
        {
            string issuer = "gdms";
            if(payload.issuer!=null)
                issuer = payload.issuer;
            Console.WriteLine("issuer: "+issuer);



            User user = _context.Users.Find(payload.userId);
            if (user == null || user.secret_key==null) //when email has userid, it might not have used the totp before, thus secret_key is not available
            {
                //build user
                User totp_user = new User(payload);

                if (user == null)
                {
                    user = totp_user;
                    _context.Users.Add(user);
                }
                else
                {
                    user.SetUser(totp_user);
                }
                _context.SaveChanges();

            }

          


            string secret_key = user.secret_key;
            var url = new TotpSetupGenerator().Generate(issuer,"Id_"+user.userId,secret_key);

            
            var inputCode = generator.Generate(secret_key);
            Console.WriteLine(inputCode+" "+validator.Validate(secret_key,inputCode));

            

            return Ok(url);
        }
        
        [HttpPost("[action]")] //{userid}
        public ActionResult<bool> CheckFirstEntry([FromBody] Payload payload)
        {
            User user = _context.Users.Find(payload.userId);
            if (user == null) return true;
            else return user.first_entry;
        }

        [HttpPost("otp")]//{userId:[userId], inputCode:[your_otpcode]}
        public ActionResult<bool> TotpValidate([FromBody]Payload payload)
        {
            User user = _context.Users.Find(payload.userId);
            if (user==null || user.secret_key==null)
            {
                return NotFound("User Not Found!! OTP Not Available! Please scan the QRcode to get otp.");
            }

            //Console.WriteLine($"{payload.inputCode}");
            var result = validator.Validate(user.secret_key, payload.inputCode,0);//看起來1分30秒內的totp都是valid  //目前無給予寬裕時間(timeToleranceInSeconds=0)
            if (result)
            {
                user.first_entry = false;
                _context.SaveChanges();
            } 
            return result;
        }

        [HttpPost("mail")] //{"userid":"1", "address":"jeffery910199@gmail.com"}
        public ActionResult SendMail(Mail mail)
        {
            try
            {
                if (mail.address != "")
                {
                    var mail_user = new User(mail);
                    User user = _context.Users.Find(mail.userId);
                    if (user == null)
                    {
                        _context.Users.Add(mail_user);
                        user = mail_user;
                    }
                    else
                    {
                        user.SetUser(mail);
                    }
                    _context.SaveChanges();

                    

                    MemoryStream stream = ConvertHtmlToImage(user.mail_otpCode);

                    /*
                    //儲存照片
                    var bytes = stream.ToArray();
                    System.IO.File.WriteAllBytes(@"C:\Users\jeffery.chen\Desktop\MFA_POC\img\image.jpg", bytes);
                    */

                    var message = new MimeMessage();// 建立郵件

                    message.From.Add(new MailboxAddress(Configuration.GetValue<string>("SMTPServer:SenderName"), "test@test.com")); //這邊email可以亂填
                    message.To.Add(new MailboxAddress("jeffery", user.address));

                    // 設定郵件標題
                    message.Subject = "MIME_TEST";

                    // 使用 BodyBuilder 建立郵件內容
                    var bodyBuilder = new BodyBuilder();
                    // 設定文字內容
                    //bodyBuilder.TextBody = "MIME_TEST";       //用不到
                    
                    stream = new MemoryStream(stream.ToArray());  //由MemoryStream轉byte[],再轉MemoryStream才可用，不知為何不能直接用MemoryStream
                    var image = bodyBuilder.LinkedResources.Add("otpCode.jpg", stream);
                    image.ContentId = MimeUtils.GenerateMessageId();
                    Console.WriteLine(image.ContentId);
                    /*
                    MemoryStream destination = new MemoryStream(stream.ToArray());
                    bodyBuilder.Attachments.Add("otpCode.jpg", destination);;*/

                    bodyBuilder.HtmlBody = string.Format(@"<p> Your OTP code is {0}, please enter the code to validate your identification </p > <img src=""cid:{1}"">", user.mail_otpCode, image.ContentId);

                    // 設定郵件內容
                    message.Body = bodyBuilder.ToMessageBody();

                    using (var client = new SmtpClient())
                    {
                        var hostUrl = "smtp.gmail.com"; 
                        var port = 465;
                        var useSsl = true;  //gmail預設開啟ssl驗證
                        /*
                        var hostUrl = "smtp.office365.com";
                        var port = 465;
                        var useSsl = true;*/

                        // 連接 Mail Server (郵件伺服器網址, 連接埠, 是否使用 SSL)
                        client.Connect(hostUrl, port, useSsl);

                        // 如果需要的話，驗證一下
                        client.Authenticate(Configuration.GetValue<string>("SMTPServer:SenderMail"), Configuration.GetValue<string>("SMTPServer:SenderPassword")); //這裡要填正確的帳號跟密碼  //if error---> need "Access for less secure apps" to Enabled ###一般帳號已經不提供支援，不能開啟           本期限不適用於 Google Workspace 或 Google Cloud Identity 客戶

                        // 寄出郵件
                        client.Send(message);

                        // 中斷連線
                        client.Disconnect(true);
                    }
                    message.Dispose();
                    
                    //After how much time the otp code would be invalid( not available )
                    Thread t = new Thread(() =>
                    {
                        int seconds = 10; //sleep for how many seconds
                        Thread.Sleep(seconds * 1000);
                        Console.WriteLine($"user_otp:{user.mail_otpCode}");
                        DeleteMailInfo(user);
                    });
                    t.Start();

                    return Ok(JsonSerializer.Serialize("寄件成功!"));
                }
                else throw new Exception("address is blank!");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e);
            }
            
        }

        [NonAction]
        public void DeleteMailInfo(User user)
        {
            var scope = _services.CreateScope();
            UsersDbContext _context = scope.ServiceProvider.GetRequiredService<UsersDbContext>();


            var user2remove = _context.Users.Find(user.userId);

            if (user2remove == null || user2remove.mail_otpCode == null) 
            {
                Console.WriteLine("user has been removed.");
            }
            else
            {
                if (user2remove.mail_otpCode == user.mail_otpCode)   //mail_optCode could change once the mail is resent.
                {
                    if (user2remove.secret_key == null)
                        _context.Users.Remove(user2remove);
                    else
                    {
                        user2remove.address = null;
                        user2remove.mail_otpCode = null;
                    }
                }
            }
            _context.SaveChanges();
            scope.Dispose();
        }


        [HttpPost("[action]")] //{   "userId":"1",   "inputCode":"OY5MVEMM"}
        public ActionResult EmailValid([FromBody] Mail mail)
        {
            User user = _context.Users.Find(mail.userId);
            if (user == null || user.mail_otpCode == null)
            {
                return NotFound("User not Found!! No otp available, please type your email to get one.");
            }
            

            
            if (user.mail_otpCode.Equals(mail.inputCode))
            {

                DeleteMailInfo(user);
                                
                return Ok(true);
            }
            else return Ok(false);
        }

        [HttpGet("img")]
        public ActionResult getImg()
        {
            string otpCode = "MWrr0mPt";
            MemoryStream stream = ConvertHtmlToImage(otpCode);

            
            //儲存照片
            var bytes = stream.ToArray();
            System.IO.File.WriteAllBytes(@"C:\Users\jeffery.chen\Desktop\MFA_POC\img\image.jpg", bytes);
            
            return Ok();
        }

        public MemoryStream ConvertHtmlToImage(string otpCode)
        {
            MemoryStream stream = new();
            var converter = new HtmlConverter();
            var html = @"<html>
                            <link href='https://fonts.googleapis.com/css?family=Allerta Stencil' rel='stylesheet'>
                            <style >  h1 { font-family: 'Allerta Stencil';font-size: 3cm; margin: 0 auto } </style > 
                            <body>" +
                                @$"<h1>{otpCode}</h1>" + @"
                            </body>
                        </html>";
            var bytes = converter.FromHtmlString(html, 512);
            stream.Write(bytes, 0, bytes.Length);

            return stream;
            //string path = @"C:\Users\jeffery.chen\Desktop\MFA_POC\img\image.jpg";
            /*using (FileStream outputFileStream = new FileStream(path, FileMode.Create))
            {
                stream.CopyTo(outputFileStream);
            }*/

        }

    }
}
