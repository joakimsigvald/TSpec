const swedishKronor = new Intl.NumberFormat('sv', { style: 'currency', currency: 'SEK', maximumFractionDigits: 0 });

export const kronor = (amount: number) => swedishKronor.format(amount);
