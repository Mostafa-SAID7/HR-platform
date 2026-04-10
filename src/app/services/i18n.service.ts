import { Injectable, signal, effect, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

type Language = 'en' | 'ar';

/**
 * Internationalization Service
 *
 * Manages language switching between English and Arabic.
 * Loads translation data from JSON assets.
 * Persists language preference to local storage.
 * Handles RTL layout switching for Arabic.
 *
 * Requirements: 2.3, 7.2, 9.1
 */
@Injectable({
  providedIn: 'root',
})
export class I18nService {
  private readonly http = inject(HttpClient);
  private readonly LANGUAGE_KEY = 'app-language';
  private readonly DEFAULT_LANGUAGE: Language = 'en';

  language = signal<Language>(this.getStoredLanguage());
  isRTL = signal<boolean>(this.language() === 'ar');
  private translations = signal<Record<string, any>>({});

  constructor() {
    // Update RTL and translations when language changes
    effect(() => {
      const lang = this.language();
      this.isRTL.set(lang === 'ar');
      this.applyLanguageToDOM(lang);
      this.persistLanguage(lang);
      this.loadTranslations(lang);
    });
  }

  private getStoredLanguage(): Language {
    const stored = localStorage.getItem(this.LANGUAGE_KEY);
    return (stored as Language) || this.DEFAULT_LANGUAGE;
  }

  private persistLanguage(language: Language): void {
    localStorage.setItem(this.LANGUAGE_KEY, language);
  }

  private applyLanguageToDOM(language: Language): void {
    const htmlElement = document.documentElement;
    htmlElement.lang = language;
    htmlElement.dir = language === 'ar' ? 'rtl' : 'ltr';

    if (language === 'ar') {
      htmlElement.classList.add('rtl');
      htmlElement.classList.remove('ltr');
    } else {
      htmlElement.classList.add('ltr');
      htmlElement.classList.remove('rtl');
    }
  }

  private async loadTranslations(language: Language): Promise<void> {
    try {
      const data = await firstValueFrom(this.http.get(`assets/i18n/${language.toLowerCase()}.json`));
      this.translations.set(data as Record<string, any>);
    } catch (error) {
      console.error(`[I18nService] Failed to load translations for ${language}:`, error);
    }
  }

  setLanguage(language: Language): void {
    this.language.set(language);
  }

  getLanguage(): Language {
    return this.language();
  }

  isArabic(): boolean {
    return this.language() === 'ar';
  }

  isEnglish(): boolean {
    return this.language() === 'en';
  }

  /**
   * Translates a key using nested object access
   * e.g. translate('app.title')
   */
  translate(key: string): string {
    const keys = key.split('.');
    let result: any = this.translations();

    for (const k of keys) {
      if (result && result[k] !== undefined) {
        result = result[k];
      } else {
        return ''; // Return empty so template || fallbacks work
      }
    }

    return typeof result === 'string' ? result : '';
  }
}
