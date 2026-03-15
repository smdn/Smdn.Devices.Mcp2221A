#!/bin/bash
base=$(cd $(dirname $0) && pwd)
domain=$1

virsh detach-device --current --file ${base}/hostdev-MCP2221A.xml ${domain}