import { Component } from '@angular/core';
import { AppSettings } from '../../config/app-settings';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.css']
})
export class AboutComponent {
  appName = 'SDMS';
  appVersion = '1.0.0';
  authUrl = AppSettings.SDMS_AuthenticationWebApp_url;
  b2cUrl = AppSettings.SDMS_B2CWebApp_url;
  environment = environment.production ? 'Production' : 'Development';
  buildDate = new Date();

  constructor() {}
}

