import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_service/members.service';
import { MemberCardComponent } from '../member-card/member-card.component';
import { PaginationModule } from 'ngx-bootstrap/pagination';
import { AccountService } from '../../_service/account.service';
import { UserParams } from '../../_models/userParams';
import { FormsModule } from '@angular/forms';
import { ButtonsModule } from 'ngx-bootstrap/buttons';

@Component({
  standalone: true,
    selector: 'app-member-list',
    imports: [MemberCardComponent, PaginationModule, FormsModule, ButtonsModule],
    templateUrl: './member-list.component.html',
    styleUrl: './member-list.component.css'
})
export class MemberListComponent implements OnInit {
  private accountService = inject(AccountService); 
  memberService = inject(MembersService);
  userParams = new UserParams(this.accountService.currentUser());
  genderList = [{value: 'male', display:'Males'}, {value: 'female', display: 'Female'}]


   ngOnInit(): void {
     if (!this.memberService.paginatedResult()) this.loadMembers();
   }
   loadMembers() {
    this.memberService.getMembers();
   }
   resetFilters() {
     this.memberService.resetUserParams();
     this.loadMembers();
   }

   pageChanged(event: any) {
     if(this.memberService.userParams().pageNumber != event.page) {
      this.memberService.userParams().pageNumber = event.page;
      this.loadMembers();
     }
   }
}
