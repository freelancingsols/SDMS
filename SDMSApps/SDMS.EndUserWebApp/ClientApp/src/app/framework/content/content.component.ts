import { Component, OnInit } from '@angular/core';

declare var particlesJS: any;

@Component({
  selector: 'app-content',
  templateUrl: './content.component.html',
  styleUrls: ['./content.component.css']
})
export class ContentComponent implements OnInit {

  constructor() { }

  ngOnInit() {
    // particlesJS.load('particles-js', 'particles.json', null);
    particlesJS.load('particles-js', 'assets/data/particles1.json', function () { console.log('callback - particles.js config loaded'); });
  }

}
