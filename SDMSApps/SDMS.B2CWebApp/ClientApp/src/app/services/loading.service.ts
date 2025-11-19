import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class LoadingService {
  private loadingSubject = new BehaviorSubject<boolean>(false);
  private loadingCount = 0;
  private loadingMessageSubject = new BehaviorSubject<string | null>(null);
  
  public readonly loading$: Observable<boolean> = this.loadingSubject.asObservable();
  public readonly loadingMessage$: Observable<string | null> = this.loadingMessageSubject.asObservable();

  /**
   * Show loading indicator
   */
  show(message?: string): void {
    this.loadingCount++;
    this.loadingSubject.next(true);
    if (message) {
      this.loadingMessageSubject.next(message);
    }
  }

  /**
   * Hide loading indicator
   */
  hide(): void {
    this.loadingCount = Math.max(0, this.loadingCount - 1);
    if (this.loadingCount === 0) {
      this.loadingSubject.next(false);
      this.loadingMessageSubject.next(null);
    }
  }

  /**
   * Check if currently loading
   */
  isLoading(): boolean {
    return this.loadingSubject.value;
  }

  /**
   * Reset loading state (force hide)
   */
  reset(): void {
    this.loadingCount = 0;
    this.loadingSubject.next(false);
    this.loadingMessageSubject.next(null);
  }

  /**
   * Execute an async operation with loading indicator
   */
  async executeWithLoading<T>(
    operation: () => Promise<T>,
    message?: string
  ): Promise<T> {
    try {
      this.show(message);
      return await operation();
    } finally {
      this.hide();
    }
  }
}

