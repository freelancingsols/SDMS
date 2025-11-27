/**
 * Notification-related interfaces
 */

export interface NotificationOptions {
  duration?: number;
  action?: string;
  horizontalPosition?: 'start' | 'center' | 'end' | 'left' | 'right';
  verticalPosition?: 'top' | 'bottom';
  panelClass?: string | string[];
}

export type NotificationType = 'success' | 'error' | 'warning' | 'info' | 'default';

