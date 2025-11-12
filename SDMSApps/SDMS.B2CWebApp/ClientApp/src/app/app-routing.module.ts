import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TestComponent } from './Components/test/test.component';
import { AuthorizeGuard } from './api-authorization/authorize.guard';
import { LoginComponent } from './api-authorization/login/login.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'auth-callback', component: TestComponent }, // OAuth callback - OAuthService handles it automatically
  { path: 'test', component: TestComponent, canActivate: [AuthorizeGuard] },
  { path: '', redirectTo: '/test', pathMatch: 'full' },
  { path: '**', redirectTo: '/test' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
