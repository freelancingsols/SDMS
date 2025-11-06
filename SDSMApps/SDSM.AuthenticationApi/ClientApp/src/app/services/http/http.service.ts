import { Component, Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { catchError, map } from 'rxjs/operators';
import { throwError } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';

@Injectable({
  providedIn: 'root'
})
export class HttpService {

  constructor(private http: HttpClient, public dialog: MatDialog) { }

  // post(path:string ,request : any) {
  //       return this.http.post<any>(path,request,{ observe: 'response' })
  //           .pipe(map(response => {
  //               return response;
  //           },
  //           catchError(err => this.handleError(err))));
  //   }
  post(path: string, request: any,headers:{[name: string]: string }={},withCredentials=false) {
    return this.http.post<any>(path, request, 
      {
         headers:new HttpHeaders( headers ), observe: 'response',withCredentials:withCredentials
      })
      .pipe(catchError(err => this.handleError(err)));
  }
  get(path:string) {
      return this.http.get<any>(path)
          .pipe(map(response => {
              return response;
          }));
  }

  private handleError(error: HttpErrorResponse) {
    if (error.error instanceof ErrorEvent) {
      // A client-side or network error occurred. Handle it accordingly.
      console.error('An error occurred:', error.error.message);
    } else {
      // The backend returned an unsuccessful response code.
      // The response body may contain clues as to what went wrong.
      console.error(
        `Backend returned code ${error.status}, ` +
        `body was: ${error.error}`);
    }
    this.dialog.open(DialogElementsExampleDialog);
    // Return an observable with a user-facing error message.
    return throwError(
      'Something bad happened; please try again later.');
  }
}


@Component({
  selector: 'dialog-elements-example-dialog',
  templateUrl: 'dialog-elements-example-dialog.html',
})
export class DialogElementsExampleDialog {}
