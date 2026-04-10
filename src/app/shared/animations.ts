import { trigger, transition, style, animate, query, group } from '@angular/animations';

/**
 * Shared Animations for HR Analytics Platform
 * 
 * Includes:
 * - fadeIn: Simple fade entrance
 * - slideInUp: Slide up and fade in entrance
 * - routeAnimation: Smooth route transition for the main dashboard
 */

export const fadeIn = trigger('fadeIn', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(5px)' }),
    animate('400ms ease-out', style({ opacity: 1, transform: 'translateY(0)' })),
  ]),
  transition('* => *', [
    style({ opacity: 0.8, filter: 'blur(4px)' }),
    animate('300ms ease-in-out', style({ opacity: 1, filter: 'blur(0)' })),
  ]),
]);

export const slideInUp = trigger('slideInUp', [
  transition(':enter', [
    style({ opacity: 0, transform: 'translateY(20px)' }),
    animate('500ms cubic-bezier(0.35, 0, 0.25, 1)', style({ opacity: 1, transform: 'translateY(0)' })),
  ]),
]);

export const routeAnimation = trigger('routeAnimation', [
  transition('* <=> *', [
    style({ position: 'relative' }),
    query(':enter, :leave', [
      style({
        position: 'absolute',
        top: 0,
        left: 0,
        width: '100%',
        opacity: 0,
      })
    ], { optional: true }),
    query(':enter', [
      style({ opacity: 0, transform: 'scale(0.98)' })
    ], { optional: true }),
    group([
      query(':leave', [
        animate('300ms ease-in', style({ opacity: 0, transform: 'scale(1.02)' }))
      ], { optional: true }),
      query(':enter', [
        animate('300ms 150ms ease-out', style({ opacity: 1, transform: 'scale(1)' }))
      ], { optional: true }),
    ]),
  ]),
]);
