import { Injectable, Inject } from '@angular/core';
import { map } from 'rxjs/operators';
import { HttpService } from '../http/http.service';

@Injectable({
  providedIn: 'root'
})
export class LoginService {
  private baseUrl: string;
  constructor(private httpService: HttpService, @Inject('BASE_URL') baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  login(request: any) {

    // return this.httpService.post(this.baseUrl+'api/Authentication/login',request);

    return this.httpService.post(this.baseUrl + 'api/Authentication/login', request,{},true).pipe(map(data => {
      if (data.status === 200 && data.body !== null && data.body.isError === false) {
        return data.body;
      } else {
        return null;
      }

    }));
  }
}

