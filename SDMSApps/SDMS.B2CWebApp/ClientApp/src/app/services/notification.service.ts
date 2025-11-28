import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig, MatSnackBarRef, TextOnlySnackBar } from '@angular/material/snack-bar';
import { NotificationOptions } from '../interfaces/notification.interface';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private readonly defaultDuration = 3000; // 3 seconds
  private readonly defaultHorizontalPosition: 'start' | 'center' | 'end' | 'left' | 'right' = 'end';
  private readonly defaultVerticalPosition: 'top' | 'bottom' = 'bottom';

  constructor(private snackBar: MatSnackBar) {}

  /**
   * Show a success notification
   */
  showSuccess(message: string, options?: NotificationOptions): MatSnackBarRef<TextOnlySnackBar> {
    const config: MatSnackBarConfig = {
      duration: options?.duration ?? this.defaultDuration,
      horizontalPosition: options?.horizontalPosition ?? this.defaultHorizontalPosition,
      verticalPosition: options?.verticalPosition ?? this.defaultVerticalPosition,
      panelClass: ['success-snackbar', ...(options?.panelClass ? (Array.isArray(options.panelClass) ? options.panelClass : [options.panelClass]) : [])],
      data: { message, type: 'success' }
    };

    return this.snackBar.open(message, options?.action ?? 'Close', config);
  }

  /**
   * Show an error notification
   */
  showError(message: string, options?: NotificationOptions): MatSnackBarRef<TextOnlySnackBar> {
    const config: MatSnackBarConfig = {
      duration: options?.duration ?? 5000, // Errors stay longer
      horizontalPosition: options?.horizontalPosition ?? this.defaultHorizontalPosition,
      verticalPosition: options?.verticalPosition ?? this.defaultVerticalPosition,
      panelClass: ['error-snackbar', ...(options?.panelClass ? (Array.isArray(options.panelClass) ? options.panelClass : [options.panelClass]) : [])],
      data: { message, type: 'error' }
    };

    return this.snackBar.open(message, options?.action ?? 'Dismiss', config);
  }

  /**
   * Show a warning notification
   */
  showWarning(message: string, options?: NotificationOptions): MatSnackBarRef<TextOnlySnackBar> {
    const config: MatSnackBarConfig = {
      duration: options?.duration ?? 4000,
      horizontalPosition: options?.horizontalPosition ?? this.defaultHorizontalPosition,
      verticalPosition: options?.verticalPosition ?? this.defaultVerticalPosition,
      panelClass: ['warning-snackbar', ...(options?.panelClass ? (Array.isArray(options.panelClass) ? options.panelClass : [options.panelClass]) : [])],
      data: { message, type: 'warning' }
    };

    return this.snackBar.open(message, options?.action ?? 'OK', config);
  }

  /**
   * Show an info notification
   */
  showInfo(message: string, options?: NotificationOptions): MatSnackBarRef<TextOnlySnackBar> {
    const config: MatSnackBarConfig = {
      duration: options?.duration ?? this.defaultDuration,
      horizontalPosition: options?.horizontalPosition ?? this.defaultHorizontalPosition,
      verticalPosition: options?.verticalPosition ?? this.defaultVerticalPosition,
      panelClass: ['info-snackbar', ...(options?.panelClass ? (Array.isArray(options.panelClass) ? options.panelClass : [options.panelClass]) : [])],
      data: { message, type: 'info' }
    };

    return this.snackBar.open(message, options?.action ?? 'OK', config);
  }

  /**
   * Show a custom notification
   */
  show(message: string, action?: string, options?: NotificationOptions): MatSnackBarRef<TextOnlySnackBar> {
    const config: MatSnackBarConfig = {
      duration: options?.duration ?? this.defaultDuration,
      horizontalPosition: options?.horizontalPosition ?? this.defaultHorizontalPosition,
      verticalPosition: options?.verticalPosition ?? this.defaultVerticalPosition,
      panelClass: options?.panelClass ? (Array.isArray(options.panelClass) ? options.panelClass : [options.panelClass]) : [],
      data: { message, type: 'default' }
    };

    return this.snackBar.open(message, action ?? 'Close', config);
  }

  /**
   * Dismiss the current snackbar
   */
  dismiss(): void {
    this.snackBar.dismiss();
  }
}

