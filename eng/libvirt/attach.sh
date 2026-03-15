#!/bin/bash
base=$(cd $(dirname $0) && pwd)
domain=$1

if virsh attach-device --current --file ${base}/hostdev-MCP2221A.xml ${domain}; then
  virsh dumpxml $domain | sed -n "/\<hostdev/,/\<\/hostdev\>/p"
fi
