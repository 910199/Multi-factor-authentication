import { Component,Input, OnInit } from '@angular/core';
import { HttpClient,HttpHeaders } from '@angular/common/http';

@Component({
  selector: 'app-qrcode',
  templateUrl: './qrcode.component.html',
  styleUrls: ['./qrcode.component.css']
})
export class QrcodeComponent implements OnInit {
  @Input() inputCode!:boolean;
  url:string="";
  result!:any;

  constructor(private http:HttpClient) {}
  
  ngOnInit(): void {
    this.getQRCode();
  }

  getQRCode():void{
    this.http.get<any>('http://localhost:8000/mfa')
    .subscribe(data=>{
      this.url = data.qrCodeImage;
      console.log("url: "+this.url);
    });
    
  }

  checkValid():void{
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });
    let options = {
      headers
    };
    this.http.post<any>('http://localhost:8000/mfa/otp',{inputCode:this.inputCode},options).subscribe(
      data=>{this.result=data;},
      error => {
        console.log(error);
        window.alert("statusText : "+error.status+" "+error.statusText+"\n only allow number as an OTP!!!");
      }
    );  
  }

}
