import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LoginService } from 'src/app/services/login/login.service';
@Component({
    selector: 'app-login',
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

    loginForm: FormGroup;
    loading = false;
    submitted = false;
    returnUrl: string;

    constructor(
        private formBuilder: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private loginService: LoginService) { }

    ngOnInit() {
        this.loginForm = this.formBuilder.group({
            username: ['', Validators.required],
            password: ['', Validators.required],
            rememberLogin: [false]
        });

        // reset login status
        // this.authenticationService.logout();

        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['ReturnUrl'];
        // var query = this.route.snapshot.queryParams['ReturnUrl'];
        // if (query.includes('connect/authorize/callback')) {
        //     var subquery =query.split('?')[1];
        //     var params = subquery.split('&');
        //     params.forEach(param => {
        //         var pair = param.split('=');
        //         if (decodeURIComponent(pair[0]) === 'ReturnUrl') {
        //             this.returnUrl= decodeURIComponent(pair[1]);
        //         }
        //     });
        // }
        // else {
        //     this.returnUrl = this.route.snapshot.queryParams['ReturnUrl'];
        // }
    }

    // convenience getter for easy access to form fields
    get form() { return this.loginForm.controls; }

    onSubmit() {
        this.submitted = true;

        // stop here if form is invalid
        if (this.loginForm.invalid) {
            return;
        }

        this.loading = true;
        const loginRequest =
            { username: this.form.username.value, password: this.form.password.value, rememberLogin: this.form.rememberLogin.value, returnUrl: this.returnUrl };
        this.loginService.login(loginRequest)
            .subscribe(
                data => {
                    // this.router.navigate([this.returnUrl]);
                    // window.open(this.returnUrl, "_self");
                    if (data) {
                        window.location = data.result;
                        //location.replace(data.result);
                    }
                },
                error => {
                    //error log
                    this.loading = false;
                });
    }

}
