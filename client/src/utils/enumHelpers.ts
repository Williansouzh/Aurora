export const enumValue = (options, key) => options.findIndex(([value]) => value === key);

export const enumLabel = (options, value) => options[value]?.[1] ?? value;
