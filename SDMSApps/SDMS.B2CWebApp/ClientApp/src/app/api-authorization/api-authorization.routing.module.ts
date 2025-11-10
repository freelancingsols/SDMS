import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { ApplicationPaths, CallbackActions } from './api-authorization.constants';
import { LoginComponent } from './login/login.component';
import { LogoutComponent } from './logout/logout.component';

const routes: Routes = [
    { path: ApplicationPaths.Register, component: LoginComponent },
    { path: ApplicationPaths.Profile, component: LoginComponent },
    { path: ApplicationPaths.Login, component: LoginComponent },
    //{ path: 'login', component: LoginComponent },
    { path: ApplicationPaths.LoginFailed, component: LoginComponent },
    { path: ApplicationPaths.LoginCallback, component: LoginComponent },
    { path: ApplicationPaths.LoginCallback+'-'+CallbackActions.PopUp, component: LoginComponent },
    { path: ApplicationPaths.LoginCallback+'-'+CallbackActions.Redirect, component: LoginComponent },
    { path: ApplicationPaths.LogOut, component: LogoutComponent },
    { path: ApplicationPaths.LoggedOut, component: LogoutComponent },
    { path: ApplicationPaths.LogOutCallback, component: LogoutComponent }
]

@NgModule({
    imports: [
        RouterModule.forChild(routes)
    ],
    exports: [
        RouterModule
    ],
    providers: []
})
export class ApiAuthorizationRoutingModule { }

