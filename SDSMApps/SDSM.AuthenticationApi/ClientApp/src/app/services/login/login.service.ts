import { Injectable,Inject } from '@angular/core';
import { HttpService } from '../http/http.service';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
private  baseUrl:string;
  constructor(private httpService : HttpService, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl=baseUrl;
  }

  login(request : any) {

    return this.httpService.post(this.baseUrl+'api/Authentication/login',request);
}
}
