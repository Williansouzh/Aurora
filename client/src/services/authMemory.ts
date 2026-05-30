let _token = null;

export const setMemoryToken = (token) => { _token = token; };
export const getMemoryToken = () => _token;
export const clearMemoryToken = () => { _token = null; };
