import { Component,Input, OnInit } from '@angular/core';
import { HttpClient,HttpHeaders } from '@angular/common/http';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-qrcode',
  templateUrl: './qrcode.component.html',
  styleUrls: ['./qrcode.component.css']
})
export class QrcodeComponent implements OnInit {
  @Input() inputCode!:number;
  url:string="";
  setupkey:string="";
  result!:any;
  userid!:string;

  constructor(private http:HttpClient,private route: ActivatedRoute) { 
    this.route.params.subscribe(
      params => { this.userid  = params['userid'];}
      );
}
  
  ngOnInit(): void {
    this.getQRCode();
  }

  getQRCode():void{
    this.http.post<any>('http://localhost:8000/mfa',{userId:this.userid})
    .subscribe(data=>{
      this.url = data.qrCodeImage;
      this.setupkey = data.manualSetupKey;
      console.log("url: "+this.url);
      console.log("setupkey: "+this.setupkey);
    });
    
  }

  checkValid():void{
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });
    let options = {
      headers
    };
    this.http.post<any>('http://localhost:8000/mfa/otp',{userId:this.userid,inputCode:this.inputCode},options).subscribe(
      data=>{this.result=data;},
      error => {
        console.log(error);
        let statement="statusText : "+error.status+" "+error.statusText+"\n";
        if(error.status!==400){
          statement = statement+error.error;
        }
        else{
          statement = statement + "only allow number as an OTP!!!";
        }
        window.alert(statement);
      }
    );  
  }

}
