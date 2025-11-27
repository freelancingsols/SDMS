import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { finalize } from 'rxjs/operators';
import { LoadingService } from './loading.service';

/**
 * HTTP Loading Interceptor
 * Automatically shows/hides loading indicator for HTTP requests
 */
@Injectable()
export class HttpLoadingInterceptor implements HttpInterceptor {
  private readonly excludedPaths = [
    '/assets/',
    '/silent-refresh.html',
    '/login-callback-silent.html'
  ];

  constructor(private loadingService: LoadingService) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Skip loading indicator for excluded paths
    if (this.shouldSkipLoading(request.url)) {
      return next.handle(request);
    }

    // Show loading indicator
    this.loadingService.show();

    return next.handle(request).pipe(
      finalize(() => {
        // Hide loading indicator when request completes
        this.loadingService.hide();
      })
    );
  }

  private shouldSkipLoading(url: string): boolean {
    return this.excludedPaths.some(path => url.includes(path));
  }
}

