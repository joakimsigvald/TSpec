import { useTranslation } from 'react-i18next';

type Props = { id?: string; code?: string };

export function ErrorMessage({ id, code }: Props) {
  const { t } = useTranslation();

  return code && (
    <p id={id} role="alert">
      {t([`errors.${code}`, 'errors.Unknown'])}
    </p>
  );
}
