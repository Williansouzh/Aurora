import { expect, test } from '@playwright/test';

// The deployed stack runs with MFA disabled (Auth__MfaEnabled=false), which is the default
// end-user flow: register and login sign the user straight in with no second factor. The
// development-only MFA test hook is intentionally excluded from Release builds, so this test
// exercises the real path rather than that hook.
test('register, login, and create an account', async ({ page }) => {
  const email = `e2e-${Date.now()}@example.com`;
  const password = 'Str0ngPass!2026';

  await page.goto('/register');
  await page.getByLabel('Nome completo').fill('E2E User');
  await page.getByLabel('E-mail').fill(email);
  await page.getByLabel('Senha').fill(password);
  await page.getByRole('button', { name: 'Criar conta' }).click();
  await expect(page).toHaveURL(/\/$/, { timeout: 30_000 });

  await page.getByRole('button', { name: 'Sair' }).first().click();
  await page.goto('/login');
  await page.getByLabel('E-mail').fill(email);
  await page.getByLabel('Senha').fill(password);
  await page.getByRole('button', { name: 'Entrar' }).click();
  await expect(page).toHaveURL(/\/$/, { timeout: 30_000 });

  await page.goto('/accounts');
  await page.getByRole('button', { name: /nova conta/i }).first().click();
  await page.getByLabel(/nome/i).fill('Conta E2E');
  await page.getByLabel(/saldo/i).fill('1000');
  await page.getByRole('button', { name: /salvar|criar/i }).click();
  await expect(page.getByText('Conta E2E')).toBeVisible();
});
