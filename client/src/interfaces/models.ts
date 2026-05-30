/**
 * Data contracts returned by the Aurora API.
 *
 * @typedef {Object} AuthUser
 * @property {string} token
 * @property {string} userId
 * @property {string} name
 * @property {string} email
 *
 * @typedef {Object} Account
 * @property {string} id
 * @property {string} name
 * @property {number} type
 * @property {number} initialBalance
 * @property {number} currentBalance
 * @property {string} color
 * @property {boolean} isArchived
 *
 * @typedef {Object} Category
 * @property {string} id
 * @property {string} name
 * @property {number} type
 * @property {string} color
 * @property {string} icon
 * @property {boolean} isDefault
 *
 * @typedef {Object} Transaction
 * @property {string} id
 * @property {string} accountId
 * @property {string} categoryId
 * @property {string} description
 * @property {number} amount
 * @property {number} type
 * @property {number} status
 * @property {string} date
 * @property {?string} dueDate
 * @property {?string} notes
 */

export {};
