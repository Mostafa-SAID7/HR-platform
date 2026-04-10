declare module 'arabic-persian-reshaper' {
  export const ArabicShaper: {
    reshape: (text: string) => string;
  };
  export const PersianShaper: {
    reshape: (text: string) => string;
  };
}
