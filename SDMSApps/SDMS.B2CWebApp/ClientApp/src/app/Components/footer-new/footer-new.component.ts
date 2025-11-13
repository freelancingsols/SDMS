import { Component } from '@angular/core';

@Component({
  selector: 'app-footer-new',
  templateUrl: './footer-new.component.html',
  styleUrls: ['./footer-new.component.css']
})
export class FooterNewComponent {
  currentYear = new Date().getFullYear();
}
