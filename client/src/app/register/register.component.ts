import { Component, EventEmitter, inject, input, output  } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccountService } from '../_service/account.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: true,
    selector: 'app-register',
    imports: [FormsModule],
    templateUrl: './register.component.html',
    styleUrl: './register.component.css'
})
export class RegisterComponent {
  private accountService = inject(AccountService);
  private toastr = inject(ToastrService);
  cancelRegister = output<boolean>();
  model: any = {};
  //@Input() usersFromHomeComponent: any;// before 17.2 v
 // @Output() cancelRegister= new EventEmitter();

  register(){
    this.accountService.register(this.model).subscribe({
     next: response =>{
      console.log(response);
      this.cancel();
     },
     error: error =>this.toastr.error(error.error)
    })
   // console.log(this.model);
  }
  cancel(){
    this.cancelRegister.emit(false);
  }

}
