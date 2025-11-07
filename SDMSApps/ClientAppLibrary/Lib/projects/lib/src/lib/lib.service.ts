import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class LibService {

constructor() { }
test(message : string): string
  {
    return message;
  }
}
