﻿using System;
using System.Collections;
using System.Net.Http.Buffered;
using System.Text;
using System.Text.Utf8;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LowAllocationWebServer.Tests
{
    [TestClass]
    public class GivenAnHttpHeaders
    {
        private string _headersString = @"Host: localhost:8080
Connection: keep-alive
Cache-Control: max-age=0
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Upgrade-Insecure-Requests: 1
User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36
Accept-Encoding: gzip, deflate, sdch
Accept-Language: en-US,en;q=0.8,pt-BR;q=0.6,pt;q=0.4
";

        private string _headerWithoutColumn = "Connection keep-alive\r\n";

        private string _headerWithoutCRLF = "Host: localhost:8080";

        private ByteSpan _headers;
        private HttpHeaders _httpHeaders;

        [TestInitialize]
        public void Setup()
        {                        
            var bytes = new UTF8Encoding().GetBytes(_headersString);            

            unsafe
            {
                fixed (byte* buffer = bytes)
                {
                    _headers = new ByteSpan(buffer, bytes.Length);
                }
            }

            _httpHeaders = new HttpHeaders(_headers);
        }

        [TestMethod]
        public void It_counts_the_number_of_headers_correctly()
        {            
            _httpHeaders.Count.Should().Be(8);
        }

        [TestMethod]
        public void It_can_get_the_value_of_a_particular_header()
        {
            _httpHeaders["Host"].ToString().Should().Be(" localhost:8080");
        }

        [TestMethod]
        public void It_returns_empty_string_when_header_is_not_present()
        {
            ((IEnumerable)_httpHeaders["Content-Length"]).Should().BeEmpty();
        }

        [TestMethod]
        public void Its_enumerator_Current_returns_the_same_item_until_MoveNext_gets_called()
        {
            var enumerator = _httpHeaders.GetEnumerator();

            enumerator.MoveNext();

            var current = enumerator.Current;
            current.Should().Be(enumerator.Current);

            enumerator.MoveNext();

            current.Should().NotBe(enumerator.Current);
        }

        [TestMethod]
        public void Its_Enumerator_iterates_through_all_headers()
        {
            var count = 0;
            foreach (var httpHeader in _httpHeaders)
            {
                count++;
            }

            count.Should().Be(8);
        }

        [TestMethod]
        public void It_parsers_Utf8String_as_well()
        {
            var httpHeader = new HttpHeaders(new Utf8String(new UTF8Encoding().GetBytes(_headersString)));

            httpHeader.Count.Should().Be(8);
        }

        [TestMethod]
        public void String_without_column_throws_ArgumentException()
        {
            var httpHeader = new HttpHeaders(new Utf8String(new UTF8Encoding().GetBytes(_headerWithoutColumn)));

            Action action = () => { var count = httpHeader.Count; }; 

            action.ShouldThrow<ArgumentException>();
        }

        [TestMethod]
        public void String_without_carriage_return_and_line_feed_throws_ArgumentException()
        {
            var httpHeader = new HttpHeaders(new Utf8String(new UTF8Encoding().GetBytes(_headerWithoutCRLF)));

            Action action = () => { var count = httpHeader.Count; };

            action.ShouldThrow<ArgumentException>();
        }
    }
}
