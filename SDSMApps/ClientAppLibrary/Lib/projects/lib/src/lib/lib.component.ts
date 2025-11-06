import { Component, OnInit } from '@angular/core';
import { LibService } from './lib.service';

@Component({
  selector: 'lib-Lib',
  template: `
    <p>
      lib works!
    </p>
    <p>{{message$}}</p>
  `,
  styles: []
})
export class LibComponent implements OnInit {
  message$ :string; 
  constructor(private libService: LibService) { }

  ngOnInit() {
    this.message$=this.libService.test('testing 123');
  }

}
