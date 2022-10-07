using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Totp;
using MFA_POC.Model;
using MimeKit;
using MailKit.Net.Smtp;
using System.Text.Json;
using System.IO;
using MimeKit.Utils;
using CoreHtmlToImage;

namespace MFA_POC.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MFAController : ControllerBase
    {
        public MFAController(Dictionary<string, User> _users) { 
            this.generator = new TotpGenerator();
            this.validator = new TotpValidator(this.generator);
            this.secret_key = "1220";
            this.users = _users;
        }
        private TotpGenerator generator { get; set; }
        private TotpValidator validator { get; set; }
        private string secret_key { get; set; }
        private Dictionary<string,User> users { get; set; }


        [HttpGet]
        public ActionResult GetURL()
        {
            
            var url = new TotpSetupGenerator().Generate("myapp","jeffery",secret_key);

            //var output = generator.Generate(secret_key);
            //var result = validator.Validate(secret_key, output);
            var inputCode = generator.Generate(secret_key);
            Console.WriteLine(inputCode+" "+validator.Validate(secret_key,inputCode));
            return Ok(url);
        }

        [HttpPost("otp")]//{inputCode:[your_otpcode]}
        public ActionResult<bool> TotpValidate([FromBody]Payload payload)
        {
            //Console.WriteLine($"{payload.inputCode}");
            var result = validator.Validate(secret_key, payload.inputCode);//看起來1分30秒內的totp都是valid
            return result;
        }

        [HttpPost("mail")] //{"userid":"1", "address":"jeffery910199@gmail.com"}
        public ActionResult SendMail(Mail mail)
        {
            try
            {
                if (mail.address != "")
                {
                    var user = new User(mail);
                    users[user.userId] = user;
                    MemoryStream stream = ConvertHtmlToImage(user.otpCode);

                    /*
                    //儲存照片
                    var bytes = stream.ToArray();
                    System.IO.File.WriteAllBytes(@"C:\Users\jeffery.chen\Desktop\MFA_POC\img\image.jpg", bytes);
                    */

                    var message = new MimeMessage();// 建立郵件

                    message.From.Add(new MailboxAddress("sender_Test", "test@test.com")); //這邊email可以亂填
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

                    bodyBuilder.HtmlBody = string.Format(@"<p> Your OTP code is {0}, please enter the code to validate your identification </p > <img src=""cid:{1}"">", user.otpCode, image.ContentId);

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
                        client.Authenticate("jeffery910199.dif03@nctu.edu.tw", "A128439448"); //這裡要填正確的帳號跟密碼

                        // 寄出郵件
                        client.Send(message);

                        // 中斷連線
                        client.Disconnect(true);
                    }
                    message.Dispose();
                    return Ok(JsonSerializer.Serialize("寄件成功!"));
                }
                else throw new Exception("address does not exist!");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e);
            }
            
        }

        [HttpPost("[action]")] //{   "userId":"1",   "inputCode":"OY5MVEMM"}
        public ActionResult EmailValid([FromBody] Mail mail)
        {
            User user = null;
            try
            {
                user = users[mail.userId];
            }
            catch { return NotFound("User not Found!! No otp available, please type your email to get one."); }
            
            if (user.otpCode.Equals(mail.inputCode))
            {
                users.Remove(user.userId);
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
