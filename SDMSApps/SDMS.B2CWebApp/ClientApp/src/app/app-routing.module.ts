import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TestComponent } from './components/test/test.component';
import { AuthorizeGuard } from './auth/authorize.guard';
import { LoginComponent } from './auth/login/login.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  // OAuth callback routes - LoginComponent handles these
  { path: 'login-callback-redirect', component: LoginComponent },
  { path: 'login-callback-popup', component: LoginComponent },
  { path: 'auth-callback', component: LoginComponent }, // Also support /auth-callback for compatibility
  { path: 'test', component: TestComponent, canActivate: [AuthorizeGuard] },
  { path: '', redirectTo: '/test', pathMatch: 'full' },
  { path: '**', redirectTo: '/test' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
