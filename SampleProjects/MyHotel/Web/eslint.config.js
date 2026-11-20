import js from '@eslint/js';
import stylistic from '@stylistic/eslint-plugin';
import reactHooks from 'eslint-plugin-react-hooks';
import { defineConfig, globalIgnores } from 'eslint/config';
import tseslint from 'typescript-eslint';

// The rules CODING-STANDARD-FRONTEND.md states that a tool can check, by their labels there.
export default defineConfig(
  globalIgnores(['dist']),
  js.configs.recommended,
  tseslint.configs.recommended,
  reactHooks.configs.flat.recommended,
  {
    plugins: { '@stylistic': stylistic },
    linterOptions: { reportUnusedDisableDirectives: 'error' },
    rules: {
      // 2a
      '@typescript-eslint/no-explicit-any': 'error',
      '@typescript-eslint/no-non-null-assertion': 'error',
      '@typescript-eslint/consistent-type-assertions': ['error', { assertionStyle: 'never' }],
      // 2b
      '@typescript-eslint/consistent-type-definitions': ['error', 'type'],
      // 6
      '@stylistic/space-infix-ops': 'error',
      '@stylistic/keyword-spacing': 'error',
      '@stylistic/comma-spacing': 'error',
      '@stylistic/arrow-spacing': 'error',
      '@stylistic/semi': 'error',
      '@stylistic/quotes': ['error', 'single', { avoidEscape: true }],
      '@stylistic/no-multi-spaces': 'error',
      '@stylistic/no-trailing-spaces': 'error',
      '@stylistic/eol-last': 'error',
    },
  },
);
