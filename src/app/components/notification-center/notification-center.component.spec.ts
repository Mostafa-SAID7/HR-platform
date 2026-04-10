import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotificationCenterComponent, Notification } from './notification-center.component';
import { FormsModule } from '@angular/forms';
import { vi } from 'vitest';

describe('NotificationCenterComponent', () => {
  let component: NotificationCenterComponent;
  let fixture: ComponentFixture<NotificationCenterComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotificationCenterComponent, FormsModule],
    }).compileComponents();

    fixture = TestBed.createComponent(NotificationCenterComponent);
    component = fixture.componentInstance;
    
    // Mock initial notifications
    const mockNotifs: Notification[] = [
      { id: '1', title: 'Test', message: 'Hello', read: false, type: 'info', timestamp: new Date().toISOString() },
      { id: '2', title: 'Test 2', message: 'Read already', read: true, type: 'success', timestamp: new Date().toISOString() }
    ];
    component.notifications.set(mockNotifs);
    
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should correctly calculate unread count', () => {
    expect(component.unreadCount()).toBe(1);
  });

  it('should mark an item as read', () => {
    const emitSpy = vi.spyOn(component.notificationRead, 'emit');
    component.markAsRead('1');
    expect(component.unreadCount()).toBe(0); // Because '1' was the only unread
    expect(emitSpy).toHaveBeenCalledWith('1');
  });

  it('should map markAllAsRead correctly to all items', () => {
    component.markAllAsRead();
    const notifs = component.notifications();
    expect(notifs.every(n => n.read)).toBe(true);
    expect(component.unreadCount()).toBe(0);
  });

  it('should remove dismissed notification', () => {
    const emitSpy = vi.spyOn(component.notificationDismissed, 'emit');
    component.dismissNotification('1');
    const notifs = component.notifications();
    expect(notifs.length).toBe(1);
    expect(notifs[0].id).toBe('2');
    expect(emitSpy).toHaveBeenCalledWith('1');
  });

  it('should save preferences when options are toggled', () => {
    const emitSpy = vi.spyOn(component.preferencesChanged, 'emit');
    component.preferences.pushNotifications = false;
    component.savePreferences();
    
    expect(emitSpy).toHaveBeenCalledWith(component.preferences);
    const lsPrefs = JSON.parse(localStorage.getItem('notificationPreferences') || '{}');
    expect(lsPrefs.pushNotifications).toBe(false);
  });
});
