import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { TestComponent } from './components/test/test.component';
import { LandingComponent } from './components/landing/landing.component';
import { AuthorizeGuard } from './auth/authorize.guard';
import { LoginComponent } from './auth/login/login.component';

const routes: Routes = [
  { path: '', component: LandingComponent }, // Landing page in center canvas - no auth required
  { path: 'login', component: LoginComponent },
  // OAuth callback routes - LoginComponent handles these
  { path: 'login-callback-redirect', component: LoginComponent },
  { path: 'login-callback-popup', component: LoginComponent },
  { path: 'auth-callback', component: LoginComponent }, // Also support /auth-callback for compatibility
  { path: 'test', component: TestComponent, canActivate: [AuthorizeGuard] }, // Test component in center canvas - requires auth
  { path: '**', redirectTo: '' } // Redirect unknown routes to landing page
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
