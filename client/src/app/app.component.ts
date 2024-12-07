import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { NavComponent } from "./nav/nav.component";
import { AccountService } from './_service/account.service';

@Component({
  standalone: true,
    selector: 'app-root',
    imports: [RouterOutlet, NavComponent],
    templateUrl: './app.component.html',
    styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  private accountService=inject(AccountService);
  
  ngOnInit(): void {
   this.setCurrentUser();
  }
  setCurrentUser() {
    const userString= localStorage.getItem('user');
    if(!userString) return;
    const user= JSON.parse(userString);
    this.accountService.currentUser.set(user);
  }
  
}
