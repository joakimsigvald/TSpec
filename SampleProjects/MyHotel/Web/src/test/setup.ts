import '@testing-library/jest-dom/vitest';
import { cleanup } from '@testing-library/react';
import { afterEach, vi } from 'vitest';
import '../i18n';

// jsdom 29 has no modal dialogs; an open one is as near as it gets.
HTMLDialogElement.prototype.showModal = function showModal() {
  this.setAttribute('open', '');
};

afterEach(() => {
  cleanup();
  vi.unstubAllGlobals();
});
