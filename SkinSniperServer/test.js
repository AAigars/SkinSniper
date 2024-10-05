const initCycleTLS = require("cycletls");

const sendRequest = async (cycleTLS, method, route, body = "") => {
  return cycleTLS(
    `https://skinport.com/api/${route}`,
    {
      body,
      ja3: "772,4865-4866-4867-49195-49199-49196-49200-52393-52392-49171-49172-156-157-47-53,65037-17513-35-13-45-10-23-11-27-18-43-16-65281-51-5-0,25497-29-23-24,0",
      userAgent:
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36",
      headers: {
        Accept: "application/json, text/plain, */*",
        "Accept-Encoding": "gzip, deflate, br, zstd",
        "Accept-Language": "en-GB,en-US;q=0.9,en;q=0.8",
        "Content-Type": "application/x-www-form-urlencoded",
        Referer: "https://skinport.com/cart",
        Cookie:
          "i18n=en; connect.sid=s%3AEvcM2uFLmShGdnIvE7GAlkRD1Qt_f3Nj.OL0pVMhpZqVrqJLrJqDRDu4iNuzs8xhyO3w7SHh1hCE; _csrf=iAVizMKwT_T5erbw_3Tl0umE; scid=eyJlbmMiOiJBMTI4Q0JDLUhTMjU2IiwiYWxnIjoiQTE5MktXIn0.bnMl5PCswSfICk8xohNWUuXQUgYsMYRzWU9rxomFrsJOcvG8Ttm3FQ.V8HgUOcDUq5WhFnxyTE4uw.09EoipCl2lTqqUF9NKFs9ps9OrvSKyNN5qdXx84U4gmui-xaMpUIptPR_pTI2tXfKQ-e9eFBPFBdm9RFpDPOuhrQHsc60Bg-q4OmHEkKNVtgLik1TCA_XEubw419LGb4FSMdTz8IZCsFYbiOf8xlVZjLcRKaPRSSDHEi9lMkoVZDC5Baem-wACqj-Hnj7_OqzNypE6o-BZ8wJ5-AqjAXfWAN5fY-2fPa2hr4lT6u2dSIP55J3JgmJsmykmMMWTX4noO7WMrXyrgdgaQlLS6i-E-lr_rzbQE-oJMlaj29jRFTSGCHudld_O49KAx_Yt5_dPFRwSto10k3Wepw22vgLReXyq5HIQzjsnTy7__SHv_gR172vh0dGP9dWc2mNELzA1-BCtvh5NGJDNxJq9KGSgobA_inx_D1_ivGR7YawpeGX5V87nx-an-EvAdiIJ7nC1Cs4r7u05n49EF5WuPbir47S0BgC2i9m6p76-LfrvuMh-seoan0zlNkpNO5SqQyIvPoWtgsnoAujMOmTY0T0Bz15shC-2sUcwIAHqq7NOiigcDeT771yj0kwk0NQoxvj00PEMnbNh8jspHvPrL_T0OLdH3oHg_isJ7JSlb3Ug1tljXn3usZfZEo1GDNERbURcPZKcqtCB45ZOlcwJkybLnScKf0RO_h2bL0SVYwp27tkK7_X3pNjBkyWqs2buaW.Es1yGHlbPwT6MYPrUUL8Ag; cf_clearance=oJUPPJR.IoVPT8vnDzvAxAt4vz9q90PgpqYiphEamlM-1728088572-1.2.1.1-joWBBd7WsTf6MJ_5_ugPF4GRfI5y7Gt8rC21dmmHFQJxz6Qdzbq50eXzAJLYS3pzk_6pTotz7wd7QLTiyH9rfMKs0KjijLtMewigq89afuO.QIyFJxsb9d4_1_3L3XgY0tYI_3D4Swnt0MyZZBOifrvqSYMp6uKv0QZCsDaCnVfi2ks2wTDWEGvd8xPca179gavColqj2omusJ4ZIxDKlhr4S2Qovrl4CCQOwiW1OWOoBqn.bRJyDZIE9e88a_nkpgONZS4KsD6oGff1YJBvSWg4xqRikKEpN8aBIZ_yqxCiHUNf6xN.idMvijm2DtAh71QpMvTEAzSCbQGDRWao9zKdiXOT.9gasJQig5ZVqMlF59KJShi4rF2eivppqRnU; __cf_bm=UXhrH5wM.OHMRwIL5G5rqEYzWDy2ctN_6ijLMkhEDOU-1728088572-1.0.1.1-HocMW4Yw68kuGbU5c6KAOOZQscRdCfZgnvsB0iN.EfvrQrVSF8dgEOphuDuuA7klheHGZfwAbKseZh7_Y.C0aw",
      },
      //proxy: "http://username:password@localhost:8888",
    },
    method
  );
};

const SALE_ID = 49226140;
const SALE_PRICE = 211;

(async () => {
  const cycleTLS = await initCycleTLS();

  const tokenResponse = await fetch(
    "http://localhost:3000/cf-clearance-scraper",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify({
        mode: "turnstile-min",
        url: "https://skinport.com/",
        siteKey: "0x4AAAAAAADTS9QyreZcUSn1",
        action: "checkout",
        cData: (SALE_ID % 1e3).toString(),
      }),
    }
  );

  const { token } = await tokenResponse.json();
  console.log(token);

  /*const {
    body: { csrf },
  } = await sendRequest(cycleTLS, "get", "data");*/

  const csrf = "A6c9uwzk-EkXS--MGWtothwOZ1v0MLakQ_9s";
  console.log(csrf);

  //await new Promise((resolve) => setTimeout(resolve, 5000));

  const basketPayload = new URLSearchParams({
    "sales[0][id]": 49226141,
    "sales[0][price]": SALE_PRICE,
    _csrf: csrf,
  });

  const { body: basketResponse } = await sendRequest(
    cycleTLS,
    "post",
    "cart/add",
    basketPayload
  );

  console.log(basketResponse);

  await new Promise((resolve) => setTimeout(resolve, 3100));

  const checkoutPayload = new URLSearchParams({
    "sales[0]": SALE_ID,
    "cf-turnstile-response": token,
    _csrf: csrf,
  });

  const { body: checkoutResponse } = await sendRequest(
    cycleTLS,
    "post",
    "checkout/create-order",
    checkoutPayload
  );

  console.log(checkoutResponse);

  cycleTLS.exit();
})();
