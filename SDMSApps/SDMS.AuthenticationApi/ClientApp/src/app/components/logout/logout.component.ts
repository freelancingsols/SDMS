import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { LogoutService } from 'src/app/services/logout/logout.service';

@Component({
  selector: 'app-logout',
  templateUrl: './logout.component.html',
  styleUrls: ['./logout.component.css']
})
export class LogoutComponent implements OnInit {
  loading = false;
  logoutId: string;
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private logoutService : LogoutService
  ) 
  {

  }

  ngOnInit() {
    this.logoutId = this.route.snapshot.queryParams['returnUrl'] || '/';
  }
  logOut() {
    
    

    

    this.logoutService.logout(this.logoutId)
            .subscribe(
                data => {
                  //if (data.signOutIFrameUrl) {
                  //  var iframe = document.createElement('iframe');
                  //  iframe.width = 0;
                  //  iframe.height = 0;
                  //  iframe.class = 'signout';
                  //  iframe.src = data.signOutIFrameUrl;
                  //  document.getElementById('logout_iframe').appendChild(iframe);
                  //}
                  if (data.postLogoutRedirectUri) {
                    this.router.navigate([data.postLogoutRedirectUri]);;
                  } else {
                    //document.getElementById('bye').innerText = 'You can close this window. Bye!';
                  }
                    
                },
                error => {
                    //error log
                    this.loading = false;
                });
  }
}
