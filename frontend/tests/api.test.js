import test from 'node:test';
import assert from 'node:assert/strict';
import { getHealth } from '../js/api.js';

test('getHealth usa el URL configurado y parsea JSON', async () => {
  const previousWindow = globalThis.window;
  const previousFetch = globalThis.fetch;
  try {
    globalThis.window = { FOLLOW_MOTORS_API_URL: 'https://example.test/api/v1/' };
    globalThis.fetch = async (url, options) => {
      assert.equal(url, 'https://example.test/api/v1/health');
      assert.equal(options.method, 'GET');
      return { ok: true, json: async () => ({ status: 'ok' }) };
    };
    assert.deepEqual(await getHealth(), { status: 'ok' });
  } finally {
    globalThis.window = previousWindow;
    globalThis.fetch = previousFetch;
  }
});

test('getHealth rechaza respuestas HTTP fallidas', async () => {
  const previousWindow = globalThis.window;
  const previousFetch = globalThis.fetch;
  try {
    globalThis.window = {};
    globalThis.fetch = async () => ({ ok: false, status: 503 });
    await assert.rejects(getHealth(), /HTTP 503/);
  } finally {
    globalThis.window = previousWindow;
    globalThis.fetch = previousFetch;
  }
});
