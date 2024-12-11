import { Component, inject, OnInit } from '@angular/core';
import { MembersService } from '../../_service/members.service';
import { Member } from '../../_models/member';
import { MemberCardComponent } from '../member-card/member-card.component';

@Component({
  standalone: true,
    selector: 'app-member-list',
    imports: [MemberCardComponent],
    templateUrl: './member-list.component.html',
    styleUrl: './member-list.component.css'
})
export class MemberListComponent implements OnInit {
   members: Member[] = [];
   memberService = inject(MembersService)
   
   ngOnInit(): void {
     if (this.memberService.members().length === 0) this.loadMembers();
   }
   loadMembers() {
    this.memberService.getMembers();
   }
}
