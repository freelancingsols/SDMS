import { Injectable,Inject } from '@angular/core';
import { HttpService } from '../http/http.service';

@Injectable({
  providedIn: 'root'
})
export class RegisterService {
  private  baseUrl:string;
  constructor(private httpService : HttpService,@Inject('BASE_URL') baseUrl: string) {
    this.baseUrl=baseUrl;
  }

  register(request : any) {

    return this.httpService.post(this.baseUrl+'api/Authentication/register',request);
}
}
