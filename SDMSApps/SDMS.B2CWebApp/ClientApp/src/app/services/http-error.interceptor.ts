import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpErrorResponse, HttpEvent } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';
import { NotificationService } from './notification.service';
import { LoadingService } from './loading.service';

/**
 * HTTP Error Interceptor
 * Handles HTTP errors globally and provides retry logic
 */
@Injectable()
export class HttpErrorInterceptor implements HttpInterceptor {
  private readonly maxRetries = 2;
  private readonly retryableStatusCodes = [408, 429, 500, 502, 503, 504];

  constructor(
    private notificationService: NotificationService,
    private loadingService: LoadingService
  ) {}

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(request).pipe(
      retry({
        count: this.maxRetries,
        delay: (error: HttpErrorResponse, retryCount: number) => {
          // Only retry on retryable status codes
          if (this.retryableStatusCodes.includes(error.status)) {
            // Exponential backoff: 1s, 2s
            return new Promise(resolve => setTimeout(resolve, 1000 * retryCount));
          }
          throw error;
        }
      }),
      catchError((error: HttpErrorResponse) => {
        this.loadingService.hide();
        
        // Handle different error types
        if (error.error instanceof ErrorEvent) {
          // Client-side error
          this.notificationService.showError('A client-side error occurred. Please try again.');
        } else {
          // Server-side error
          const errorMessage = this.getErrorMessage(error);
          this.notificationService.showError(errorMessage);
        }

        return throwError(() => error);
      })
    );
  }

  private getErrorMessage(error: HttpErrorResponse): string {
    // Try to get user-friendly error message from API response
    if (error.error?.message) {
      return error.error.message;
    }

    if (error.error?.error) {
      return error.error.error;
    }

    // Map HTTP status codes to user-friendly messages
    switch (error.status) {
      case 400:
        return 'Invalid request. Please check your input and try again.';
      case 401:
        return 'Your session has expired. Please log in again.';
      case 403:
        return 'You do not have permission to perform this action.';
      case 404:
        return 'The requested resource was not found.';
      case 408:
        return 'Request timed out. Please try again.';
      case 429:
        return 'Too many requests. Please wait a moment and try again.';
      case 500:
        return 'A server error occurred. Please try again later.';
      case 502:
      case 503:
      case 504:
        return 'Service temporarily unavailable. Please try again later.';
      default:
        return 'An unexpected error occurred. Please try again.';
    }
  }
}

