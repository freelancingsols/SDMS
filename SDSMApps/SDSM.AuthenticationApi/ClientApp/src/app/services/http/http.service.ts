import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class HttpService {

  constructor(private http: HttpClient) { }

  post(path:string ,request : any) {
        return this.http.post<any>(path,request)
            .pipe(map(response => {
                return response;
            }));
    }
  get(path:string) {
      return this.http.get<any>(path)
          .pipe(map(response => {
              return response;
          }));
  }
}
