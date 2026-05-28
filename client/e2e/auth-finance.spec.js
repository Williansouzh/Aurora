import { expect, test } from '@playwright/test';

const apiBaseUrl = process.env.E2E_API_URL || 'http://localhost:8080';

test('register, login with MFA, and create an account', async ({ page, request }) => {
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
  await expect(page.getByText('Verificacao em duas etapas')).toBeVisible();

  const challengeResponse = await request.get(`${apiBaseUrl}/__test/latest-mfa-code?email=${encodeURIComponent(email)}`);
  expect(challengeResponse.ok()).toBeTruthy();
  const { code } = await challengeResponse.json();

  await page.getByLabel('Codigo').fill(code);
  await page.getByRole('button', { name: 'Verificar codigo' }).click();
  await expect(page).toHaveURL(/\/$/, { timeout: 30_000 });

  await page.goto('/accounts');
  await page.getByRole('button', { name: /nova conta/i }).first().click();
  await page.getByLabel(/nome/i).fill('Conta E2E');
  await page.getByLabel(/saldo/i).fill('1000');
  await page.getByRole('button', { name: /salvar|criar/i }).click();
  await expect(page.getByText('Conta E2E')).toBeVisible();
});
