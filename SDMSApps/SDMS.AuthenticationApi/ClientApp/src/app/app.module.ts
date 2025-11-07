import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { ReactiveFormsModule }    from '@angular/forms';
import { AppComponent } from './app.component';
import { RouterModule } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { LogoutComponent } from './components/logout/logout.component';
import { LoginService } from './services/login/login.service';
import { RegisterService } from './services/register/register.service';
import { DialogElementsExampleDialog, HttpService } from './services/http/http.service';
import { HttpClientModule } from '@angular/common/http';
import { LogoutService } from './services/logout/logout.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialogModule } from '@angular/material/dialog';
@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    LogoutComponent,
    DialogElementsExampleDialog
  ],
  imports: [
    BrowserModule,
    ReactiveFormsModule,
    AppRoutingModule,
    RouterModule,
    HttpClientModule,
    NoopAnimationsModule,
    MatDialogModule
  ],
  providers: 
  [
    LoginService,
    RegisterService,
    HttpService,
    LogoutService    
  ],
  entryComponents: [
    DialogElementsExampleDialog
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
