import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { User } from '../_models/user';
import { Router } from '@angular/router';
import { take } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {
  hubUrl = environment.hubsUrl;
  private hubConncetion?: HubConnection;
  private toastr = inject(ToastrService)
  private router =inject(Router)
  onlineUsers= signal<string[]>([]);

  createHubConnection(user: User) {
    this.hubConncetion = new HubConnectionBuilder()
    .withUrl(this.hubUrl + 'presence', {
      accessTokenFactory: () => user.token
    })
    .withAutomaticReconnect()
    .build();

    this.hubConncetion.start().catch(error => console.log(error));

    this.hubConncetion.on('UserIsOnline',username => {
      this.onlineUsers.update(users =>[...users, username]);
      //this.toastr.info(username + ' has connected');
    });

    this.hubConncetion.on('UserIsOffline',username => {
      this.onlineUsers.update(users => users.filter(x=>x !==username));
      //this.toastr.warning(username + ' has disconnected');
    });

    this.hubConncetion.on('GetOnlineUsers', username=>{
      this.onlineUsers.set(username);
    });

    this.hubConncetion.on("NewMessageRecived", ({username, knownAs}) =>{
      this.toastr.info(knownAs +' has sent you a new message! Click me to see it')
      .onTap
      .pipe(take(1))
      .subscribe(()=>this.router.navigateByUrl('/members/' + username + '?tab=Messages'))
    } )
  }

  stopHubConnection() {
    if(this.hubConncetion?.state === HubConnectionState.Connected){
      this.hubConncetion.stop().catch(error => console.log(error));
    } 
  }
}
