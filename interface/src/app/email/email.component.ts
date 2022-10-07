import { Component,Input,OnInit } from '@angular/core';
import { FormBuilder,Validators  } from '@angular/forms';
import { HttpClient,HttpHeaders} from '@angular/common/http';
import { Observable } from 'rxjs';


@Component({
  selector: 'app-email',
  templateUrl: './email.component.html',
  styleUrls: ['./email.component.css']
})
export class EmailComponent implements OnInit {
  @Input() email!:string;
  @Input() inputCode!:number;
  result:any;
  emailForm = this.formbuilder.group({
    email:['', [Validators.required,Validators.pattern("^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+$")]]
  })
  email_status = this.emailForm.get("email")?.statusChanges ;
  mailed = false;
  

  constructor(private formbuilder: FormBuilder,private http: HttpClient) { }

  ngOnInit(): void {
  }

  send_mail():void{
    this.http.post<any>('http://localhost:8000/mfa/mail',{address:this.emailForm.value.email,userid:"1"}).subscribe(
      data=>{
        this.mailed = false;
        window.alert(data);
      },
      error=>{this.mailed = false;}
    );
    this.emailForm.reset();
    this.mailed = true;
  }

  checkValid():void{
    this.http.post<any>('http://localhost:8000/mfa/emailValid',{inputCode:this.inputCode,userid:"1"}).subscribe(
      data=>{this.result=data;},
      error=>{
        let err_msg = "404 Not Found: "+error.error;
        console.log(err_msg);
        this.result = error.error;
      }
    );
  }

  get emailnoValid(){
    return this.emailForm.get("email")!.invalid && this.emailForm.get('email')!.touched;
  }
}
