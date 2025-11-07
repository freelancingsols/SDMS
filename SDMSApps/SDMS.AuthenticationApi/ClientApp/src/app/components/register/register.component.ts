import { Component, OnInit } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RegisterService } from 'src/app/services/register/register.service';
@Component({
    selector: 'app-register',
    templateUrl: './register.component.html',
    styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {

  registerForm: FormGroup;
    loading = false;
    submitted = false;
    returnUrl: string;

    constructor(
        private formBuilder: FormBuilder,
        private route: ActivatedRoute,
        private router: Router,
        private registerService: RegisterService) { }

    ngOnInit() {
        this.registerForm = this.formBuilder.group({
            username: ['', Validators.required],
            password: ['', Validators.required],
            firstName: ['', Validators.required],
            lastName: ['', Validators.required],
            rememberLogin: [false]
        });

        // reset login status
        // this.authenticationService.logout();

        // get return url from route parameters or default to '/'
        this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';
    }

    // convenience getter for easy access to form fields
    get form() { return this.registerForm.controls; }

    onSubmit() {
        this.submitted = true;

        // stop here if form is invalid
        if (this.registerForm.invalid) {
            return;
        }

        this.loading = true;
        const loginRequest =
            { 
              username: this.form.username.value, 
              password: this.form.password.value, 
              firstName: this.form.firstName.value, 
              lastName: this.form.lastName.value, 
              rememberLogin: this.form.rememberLogin.value, 
              returnUrl: this.returnUrl };
        this.registerService.register(loginRequest)
            .subscribe(
                data => {
                    this.router.navigate([this.returnUrl]);
                },
                error => {
                    //error log
                    this.loading = false;
                });
    }

}
