// ==UserScript==
// @name         Amzn Industry CliAgent
// @match        https://i.jwy3.com/*
// @match        https://i.jwy3.dev/*
// @connect      *
// @grant        GM_xmlhttpRequest
// @grant        unsafeWindow
// ==/UserScript==

(function () {
    'use strict';

    {
        const Wait
            = setInterval(() => {

                const bool
                    = unsafeWindow
                        .Module

                    && typeof
                    unsafeWindow
                        .Module
                        .SendMessage
                    === 'function'

                switch (bool) {
                    case true:
                        {
                            clearInterval
                                (Wait);

                            unsafeWindow
                                .Module
                                .SendMessage(
                                    'Canvas',
                                    'JavaScript_ClientAgent_isActive_Internal',
                                    'true'
                                );
                        }
                        break;
                }

            }, 100);
    }

    {
        unsafeWindow
            .ClientAgent
            = function
                (json) {

                const object
                    = JSON
                        .parse
                        (json);

                const event
                    = object
                        .Event;

                switch (event) {
                    case "Http":
                        {
                            const method
                                = object
                                    .Method;

                            const url
                                = object
                                    .Url;

                            switch (method) {
                                case "Get":
                                    {
                                        try {
                                            GM_xmlhttpRequest({
                                                method: "GET",
                                                url: url,

                                                onload: function (response) {

                                                    unsafeWindow
                                                        .Module
                                                        .SendMessage(
                                                            'Canvas',
                                                            'JavaScript_ClientAgent_Internal',
                                                            response
                                                                .responseText
                                                        );
                                                },

                                                onerror: function (error) {

                                                    unsafeWindow
                                                        .Module
                                                        .SendMessage(
                                                            'Canvas',
                                                            'JavaScript_ClientAgent_Error_Internal',
                                                            error
                                                                .toString()
                                                        );
                                                }
                                            });

                                        } catch (error) {

                                            unsafeWindow
                                                .Module
                                                .SendMessage(
                                                    'Canvas',
                                                    'JavaScript_ClientAgent_Error_Internal',
                                                    error
                                                        .toString()
                                                );
                                        }
                                    }
                                    break;
                                case "Post":
                                    {
                                        const payload
                                            = object
                                                .Payload;

                                        switch (payload) {
                                            case "Data":
                                                {
                                                    const data
                                                        = object
                                                            .Data;

                                                    GM_xmlhttpRequest({
                                                        method: "POST",
                                                        url: url,

                                                        headers: {
                                                            "Content-Type": "application/json"
                                                        },

                                                        data: JSON
                                                            .stringify
                                                            (data),

                                                        onload: function (response) {

                                                            unsafeWindow
                                                                .Module
                                                                .SendMessage(
                                                                    'Canvas',
                                                                    'JavaScript_ClientAgent_Internal',
                                                                    response
                                                                        .responseText
                                                                );
                                                        },

                                                        onerror: function (error) {

                                                            unsafeWindow
                                                                .Module
                                                                .SendMessage(
                                                                    'Canvas',
                                                                    'JavaScript_ClientAgent_Error_Internal',
                                                                    error
                                                                        .toString()
                                                                );
                                                        }
                                                    });
                                                }
                                                break;
                                            case "Form":
                                                {
                                                    const form
                                                        = object
                                                            .Form;

                                                    const params
                                                        = new URLSearchParams();

                                                    for (const key in form) {
                                                        params
                                                            .append
                                                            (key, form[key]);
                                                    }

                                                    GM_xmlhttpRequest({
                                                        method: "POST",
                                                        url: url,
                                                        headers: {
                                                            "Content-Type": "application/x-www-form-urlencoded"
                                                        },
                                                        data: params.toString(),

                                                        onload(response) {
                                                            unsafeWindow.Module.SendMessage(
                                                                "Canvas",
                                                                "JavaScript_ClientAgent_Internal",
                                                                response.responseText
                                                            );
                                                        },

                                                        onerror(error) {
                                                            unsafeWindow.Module.SendMessage(
                                                                "Canvas",
                                                                "JavaScript_ClientAgent_Error_Internal",
                                                                error.toString()
                                                            );
                                                        }
                                                    });
                                                }
                                                break;
                                        }
                                    }
                                    break;
                            }
                        }
                        break
                }
            };
    }
})();