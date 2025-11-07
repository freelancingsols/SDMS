import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthorizeService } from 'src/app/api-authorization/authorize.service';
@Component({
  selector: 'app-test',
  templateUrl: './test.component.html',
  styleUrls: ['./test.component.css']
})
export class TestComponent implements OnInit {
private username: string;
  constructor(private router: Router,private authorizeService: AuthorizeService,) { }

  ngOnInit() {
    this.authorizeService.getUser().subscribe(user=>
      {
        this.username=user.name;
      });
    ;
  }

  public loadTest()
  {
     this.router.navigateByUrl('/login',{
      replaceUrl: true
    })
  }
}
