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
import { HttpService } from './services/http/http.service';
import { HttpClientModule } from '@angular/common/http';
import { LogoutService } from './services/logout/logout.service';
@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    RegisterComponent,
    LogoutComponent
  ],
  imports: [
    BrowserModule,
    ReactiveFormsModule,
    AppRoutingModule,
    RouterModule,
    HttpClientModule
  ],
  providers: 
  [
    LoginService,
    RegisterService,
    HttpService,
    LogoutService    
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
