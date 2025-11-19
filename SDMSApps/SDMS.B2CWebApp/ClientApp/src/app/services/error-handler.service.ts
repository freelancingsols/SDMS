import { ErrorHandler, Injectable, NgZone } from '@angular/core';
import { NotificationService } from './notification.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(
    private notificationService: NotificationService,
    private ngZone: NgZone
  ) {}

  handleError(error: Error | any): void {
    // Run in Angular zone to ensure change detection works
    this.ngZone.run(() => {
      // Log error to console for debugging
      console.error('Global error handler:', error);

      // Extract user-friendly error message
      const userMessage = this.getUserFriendlyMessage(error);

      // Show error notification
      this.notificationService.showError(userMessage, {
        duration: 6000
      });

      // TODO: Send error to logging service/backend
      // this.logErrorToService(error);
    });
  }

  private getUserFriendlyMessage(error: Error | any): string {
    // Handle different error types
    if (error?.error?.error_description) {
      return error.error.error_description;
    }

    if (error?.error?.message) {
      return error.error.message;
    }

    if (error?.message) {
      // Map common technical errors to user-friendly messages
      const message = error.message.toLowerCase();

      if (message.includes('network') || message.includes('fetch')) {
        return 'Network error. Please check your internet connection and try again.';
      }

      if (message.includes('timeout')) {
        return 'Request timed out. Please try again.';
      }

      if (message.includes('unauthorized') || message.includes('401')) {
        return 'Your session has expired. Please log in again.';
      }

      if (message.includes('forbidden') || message.includes('403')) {
        return 'You do not have permission to perform this action.';
      }

      if (message.includes('not found') || message.includes('404')) {
        return 'The requested resource was not found.';
      }

      if (message.includes('server error') || message.includes('500')) {
        return 'A server error occurred. Please try again later.';
      }

      // Return original message if no mapping found
      return error.message;
    }

    // Default message
    return 'An unexpected error occurred. Please try again.';
  }

  // TODO: Implement error logging to backend
  // private logErrorToService(error: Error | any): void {
  //   // Send to logging service
  // }
}

