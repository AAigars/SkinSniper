const { connect } = require("puppeteer-real-browser");
const readline = require("readline");
const net = require("net");
const path = require("path");

const BASE_URL = "https://skinport.com/";
const ACCOUNT_URL = `${BASE_URL}account`;
const CONFIRM_NEW_DEVICE_URL = `${ACCOUNT_URL}/confirm-new-device`;
const SUPPORT_URL = `${BASE_URL}support/new`;

const PIPE_NAME = "SkinSniperPipe";
const EMAIL = "aldermanis.aigars@gmail.com";
const PASSWORD = '"Aigarin2202"';

const sleep = (delay) => new Promise((resolve) => setTimeout(resolve, delay));

const server = net.createServer(async (stream) => {
  console.log("New connection to server!");

  const { page, browser } = await connect({
    turnstile: true,
    connectOption: { defaultViewport: null },
    disableXvfb: false,
  });

  await page.goto(BASE_URL, { waitUntil: "networkidle2" });
  await page.evaluate(() => {
    localStorage.setItem("user", JSON.stringify({}));
    localStorage.setItem("session", JSON.stringify({ allCookiesAccepted: !0 }));
  });

  await page.goto(ACCOUNT_URL, { waitUntil: "networkidle2" });

  if (page.url !== ACCOUNT_URL) {
    await page.locator("#email").fill(EMAIL);
    await page.locator("#password").fill(PASSWORD);
    await sleep(500);

    await Promise.all([
      page.waitForNavigation(),
      page.locator(".SubmitButton").click(),
    ]);

    if (page.url() === CONFIRM_NEW_DEVICE_URL) {
      const rl = readline.createInterface({
        input: process.stdin,
        output: process.stdout,
      });

      await new Promise((resolve) => {
        rl.question("Confirm new device: ", async (url) => {
          await page.goto(url);
          rl.close();
          resolve();
        });
      });

      await page.waitForNavigation();
    }

    if (page.url() === BASE_URL) {
      await page.evaluateOnNewDocument(() => {
        window.addEventListener("message", (event) => {
          if (!event.data.scriptId || event.data.fromMain) {
            // ignore messages without scriptId and from ourselves (from main context)
            return;
          }

          const response = {
            scriptId: event.data.scriptId,
            fromMain: true,
          };
          try {
            response.result = eval(event.data.scriptText);
          } catch (err) {
            response.error = err.message;
          }

          window.postMessage(JSON.parse(JSON.stringify(response)));
        });
      });

      await page.goto(SUPPORT_URL, { waitUntil: "networkidle2" });

      await page.evaluate(() => {
        // listen for messages from main and emit custom event with a response for a specific scriptId
        window.addEventListener("message", (event) => {
          if (!(event.data.scriptId && event.data.fromMain)) {
            // ignore irrelevant messages
            return;
          }

          window.dispatchEvent(
            new CustomEvent(`scriptId-${event.data.scriptId}`, {
              detail: event.data,
            })
          );
        });

        // a helper that can be reused in other page.evaluate calls
        window.evaluateMain = (scriptFn) => {
          // generate unique scriptId for each call
          window.evaluateMainScriptId = (window.evaluateMainScriptId || 0) + 1;
          const scriptId = window.evaluateMainScriptId;
          return new Promise((resolve) => {
            // listen for the response
            window.addEventListener(
              `scriptId-${scriptId}`,
              (event) => {
                resolve(event.detail);
              },
              {
                once: true,
              }
            );

            // prepare and send a message for the main context
            let scriptText = scriptFn;
            if (typeof scriptText !== "string") {
              scriptText = `(${scriptText.toString()})()`;
            }
            window.postMessage({
              scriptId,
              scriptText,
            });
          });
        };
      });

      await page.evaluate(() =>
        window.evaluateMain(() => {
          const element = document.getElementById("cf-turnstile");
          window.turnstile.remove(element);
          window.turnstile.render(element, {
            action: "checkout",
            execution: "render",
            "refresh-expired": "auto",
            sitekey: "0x4AAAAAAADTS9QyreZcUSn1",
          });
        })
      );

      const { csrf } = await page.evaluate(async () => {
        const response = await fetch(
          "https://skinport.com/api/data?v=0970ccc23937155d5714&t=1726230019"
        );
        return await response.json();
      });

      console.log(csrf);

      stream.on("data", async (data) => {
        const view = new DataView(data.buffer);

        const id = view.getUint8();
        if (id === 0x1) {
          const saleId = view.getUint32(0x1, true);
          const salePrice = view.getUint32(0x1 + 0x4, true);

          const token = await page.evaluate(() =>
            window.evaluateMain(() =>
              window.turnstile.getResponse(
                document.getElementById("cf-turnstile")
              )
            )
          );

          await page.evaluate(
            async (csrf, token, saleId, salePrice) => {
              const basketPayload = new URLSearchParams({
                "sales[0][id]": saleId,
                "sales[0][price]": salePrice,
                _csrf: csrf,
              });

              const orderPayload = new URLSearchParams({
                "sales[0]": saleId,
                "cf-turnstile-response": token,
                _csrf: csrf,
              });

              console.log(token);

              const basketRequest = () =>
                fetch("https://skinport.com/api/cart/add", {
                  method: "POST",
                  body: basketPayload,
                });

              const orderRequest = () =>
                fetch("https://skinport.com/api/checkout/create-order", {
                  method: "POST",
                  body: orderPayload,
                });

              //await Promise.all([basketRequest(), orderRequest()]);

              orderRequest();
            },
            csrf,
            token.result,
            saleId,
            salePrice
          );

          stream.write(new Uint8Array([1]));
        }
      });
    }
  }
});

server.listen(path.join(`\\\\.\\pipe`, PIPE_NAME));
