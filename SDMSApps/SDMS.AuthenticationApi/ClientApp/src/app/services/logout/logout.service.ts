import { Injectable,Inject } from '@angular/core';
import { HttpService } from '../http/http.service';

@Injectable({
  providedIn: 'root'
})
export class LogoutService {
  private  baseUrl:string;
  constructor(private httpService : HttpService, @Inject('BASE_URL') baseUrl: string) { 
    this.baseUrl=baseUrl;
  }

  logout(logoutId : string) {

    return this.httpService.get(this.baseUrl+'api/Authentication/logout?logoutId='+logoutId);
}
}
